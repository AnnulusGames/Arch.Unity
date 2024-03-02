using System;
using System.Runtime.InteropServices;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Arch.Core;

namespace Arch.Unity.Jobs
{
    [JobProducerType(typeof(IJobArchChunkExtensions.JobArchChunkStruct<>))]
    public interface IJobArchChunk
    {
        void Execute(NativeChunk chunk);
    }

    public struct JobArchChunkHandle
    {
        public JobHandle JobHandle;
        public NativeList<GCHandle> GCHandles;
        public AllocatorHelper<RewindableAllocator> Allocator;

        public World World;
        public QueryDescription QueryDescription;

        public void Dispose()
        {
            if (GCHandles.IsCreated)
            {
                foreach (var gcHandle in GCHandles)
                {
                    GCHandlePool.Release(gcHandle);
                }
            }

            AllocatorPool.FreeAndReturn(Allocator);
        }

        static readonly MinimumList<JobArchChunkHandle> runningHandles = new();

        public static void Initialize()
        {
            for (int i = 0; i < runningHandles.Length; i++) runningHandles[i].Dispose();
            runningHandles.Clear();
        }

        public static void Register(in JobArchChunkHandle handle)
        {
            runningHandles.Add(handle);
        }

        public static void CheckHandles()
        {
            for (int i = 0; i < runningHandles.Length; i++)
            {
                var handle = runningHandles[i];
                if (handle.JobHandle.IsCompleted)
                {
                    handle.Dispose();
                    runningHandles.RemoveAtSwapback(i);
                    i--;
                }
            }
        }
    }

    public static class IJobArchChunkExtensions
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct JobWrapper<T> where T : struct, IJobArchChunk
        {
            public NativeArray<NativeChunk> ChunkArray;
            public T JobData;
        }

        internal readonly struct JobArchChunkStruct<T> where T : struct, IJobArchChunk
        {
            internal static readonly SharedStatic<IntPtr> jobReflectionData = SharedStatic<IntPtr>.GetOrCreate<JobArchChunkStruct<T>>();

            [BurstDiscard]
            internal static unsafe void Initialize()
            {
                if (jobReflectionData.Data == IntPtr.Zero)
                {
                    jobReflectionData.Data = JobsUtility.CreateJobReflectionData(typeof(JobWrapper<T>), (ExecuteJobFunction)Execute);
                }
            }

            public delegate void ExecuteJobFunction(ref JobWrapper<T> jobData, IntPtr additionalPtr, IntPtr bufferRangePatchData, ref JobRanges ranges, int jobIndex);
            public static unsafe void Execute(ref JobWrapper<T> jobData, IntPtr jobData2, IntPtr bufferRangePatchData, ref JobRanges ranges, int jobIndex)
            {
                while (true)
                {
                    if (!JobsUtility.GetWorkStealingRange(ref ranges, jobIndex, out int begin, out int end))
                    {
                        break;
                    }

                    JobsUtility.PatchBufferMinMaxRanges(bufferRangePatchData, UnsafeUtility.AddressOf(ref jobData), begin, end - begin);

                    var endThatCompilerCanSeeWillNeverChange = end;
                    for (var i = begin; i < endThatCompilerCanSeeWillNeverChange; i++)
                    {
                        var chunk = jobData.ChunkArray[i];
                        jobData.JobData.Execute(chunk);
                    }
                }
            }
        }

        public static void EarlyJobInit<T>()
            where T : struct, IJobArchChunk
        {
            JobArchChunkStruct<T>.Initialize();
        }

        static IntPtr GetReflectionData<T>()
            where T : struct, IJobArchChunk
        {
            JobArchChunkStruct<T>.Initialize();
            var reflectionData = JobArchChunkStruct<T>.jobReflectionData.Data;
            // JobValidationInternal.CheckReflectionDataCorrect<T>(reflectionData);
            return reflectionData;
        }

        public unsafe static void Run<T>(this T jobData, World world, QueryDescription query) where T : struct, IJobArchChunk
        {
            ScheduleInternal(ref jobData, world, query, ScheduleMode.Run, default).Complete();
        }

        public unsafe static void RunByRef<T>(this ref T jobData, World world, QueryDescription query) where T : struct, IJobArchChunk
        {
            ScheduleInternal(ref jobData, world, query, ScheduleMode.Run, default).Complete();
        }

        public unsafe static JobHandle Schedule<T>(this T jobData, World world, QueryDescription query, JobHandle dependsOn = new JobHandle()) where T : struct, IJobArchChunk
        {
            return ScheduleInternal(ref jobData, world, query, ScheduleMode.Single, dependsOn);
        }

        public unsafe static JobHandle ScheduleByRef<T>(this ref T jobData, World world, QueryDescription query, JobHandle dependsOn = new JobHandle()) where T : struct, IJobArchChunk
        {
            return ScheduleInternal(ref jobData, world, query, ScheduleMode.Single, dependsOn);
        }

        public unsafe static JobHandle ScheduleParallel<T>(this T jobData, World world, QueryDescription query, JobHandle dependsOn = new JobHandle()) where T : struct, IJobArchChunk
        {
            return ScheduleInternal(ref jobData, world, query, ScheduleMode.Parallel, dependsOn);
        }

        public unsafe static JobHandle ScheduleParallelByRef<T>(this ref T jobData, World world, QueryDescription query, JobHandle dependsOn = new JobHandle()) where T : struct, IJobArchChunk
        {
            return ScheduleInternal(ref jobData, world, query, ScheduleMode.Parallel, dependsOn);
        }

        unsafe static JobHandle ScheduleInternal<T>(ref T jobData, World world, QueryDescription query, ScheduleMode scheduleMode, JobHandle dependsOn) where T : struct, IJobArchChunk
        {
            var allocator = AllocatorPool.Rent();

            var nativeChunks = new NativeList<NativeChunk>(allocator.Allocator.Handle);
            var gcHandles = new NativeList<GCHandle>(allocator.Allocator.Handle);

            foreach (var chunk in world.Query(query).GetChunkIterator())
            {
                var nativeChunk = NativeChunk.Create(chunk, allocator.Allocator.Handle, ref gcHandles);
                nativeChunks.Add(nativeChunk);
            }

            var chunkJobData = new JobWrapper<T>()
            {
                ChunkArray = nativeChunks.AsArray(),
                JobData = jobData,
            };

            var scheduleParams = new JobsUtility.JobScheduleParameters(UnsafeUtility.AddressOf(ref chunkJobData), GetReflectionData<T>(), dependsOn, scheduleMode);
            var jobHandle = JobsUtility.ScheduleParallelFor(ref scheduleParams, nativeChunks.Length, 1);
   
            var handle = new JobArchChunkHandle()
            {
                JobHandle = jobHandle,
                Allocator = allocator,
                GCHandles = gcHandles,
                World = world,
                QueryDescription = query,
            };
            JobArchChunkHandle.Register(handle);

            return jobHandle;
        }
    }
}
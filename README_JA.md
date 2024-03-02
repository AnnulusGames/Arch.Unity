# Arch.Unity

[English README is here.](README.md)

Arch.UnityはC#向けのECSフレームワークである[Arch](https://github.com/genaray/Arch)をUnityに統合するための機能を提供するライブラリです。

## Why not Unity ECS?

Unityには公式に提供されるEntitiesパッケージを用いることで、Untiyエディタに統合された極めて高速なECSフレームワークを使用することができます。また、他にもEntitiesに対応した物理演算や描画機能を持つパッケージが提供されています。

しかしながらEntitiesはECSフレームワークとしてやや機能過多であり、これを導入する場合はプロジェクトの構成から大きく見直す必要があります。また現時点でUnity ECSは対応パッケージが明らかに不足しており、多くの場合においてGameObjectと相互に運用を行う"Hybrid ECS"的なアプローチが必須です。正直なところ、最適化があまり重要でない小規模〜中規模のプロジェクト(特にUnity ECSの機能が不足している2Dプロジェクト)ではUnity ECSを採用するメリットはあまりないと言って良いでしょう。

それでもなお、ECS自体を採用するメリットは存在します。UnityのUpdate関数は通常のメソッド呼び出しよりも低速であり、最適化の際に多くの開発者が独自の"UpdateManager"を作成しています。また、処理とデータが一つのComponent内に混在することによる可読性の低下も問題です。これらはデータをEntity上に載せ、メソッドをSystemとして分離することで解決できます。

ArchはC#向けのECSフレームワークであり、十分な速度を持つほか、(多少のパフォーマンス低下はありますが)参照型をComponentに使用することが可能です。また、Archのコア部分はミニマルで洗練されており、Unityへの組み込みも容易です。Arch.Unityはこれにいくつかの機能・レイヤーを追加することで、Arch ECSとUnityのスムーズな統合を実現します。

## セットアップ

### 要件

* Unity 2022.2 以上
* Burst 1.6.0以上
* Collections 2.0.0以上
* Arch 1.0.0以上
* Arch.System 1.0.0以上

### インストール

1. [Nuget For Unity](https://github.com/GlitchEnzo/NuGetForUnity)を使用してArchとArch.Systemをインストールする

<img src="https://github.com/AnnulusGames/Arch.Unity/blob/main/docs/images/img-nuget-for-unity.png" width="600">

2. Window > Package ManagerからPackage Managerを開く
3. 「+」ボタン > Add package from git URL
4. 以下のURLを入力する

```
https://github.com/AnnulusGames/Arch.Unity.git?path=src/Arch.Unity/Assets/Arch.Unity
```

## パッケージ構成

| namespace | 説明 |
| - | - |
| Arch.Unity.Conversion | GameObjectをEntityに変換する機能を提供 |
| Arch.Unity.Editor | Entityを表示するHierarchyやInspectorなどのエディタ拡張 |
| Arch.Unity.Jobs | ArchのクエリとUntiyのC# Job Systemの統合 |
| Arch.Unity.Toolkit | Arch.SystemをUnityに統合する独自の機能を提供 |

## Conversionワークフロー

GameObjectからEntityを作成するための機能として`EntityConverter`コンポーネントが提供されています。これを用いることでGameObjectをEntityに変換したり、GameObjectが持つComponentをEntityに追加して同期したりなど、GameObjectとEntityの相互運用を行うことが可能になります。

<img src="https://github.com/AnnulusGames/Arch.Unity/blob/main/docs/images/img-entity-converter-inspector.png" width="600">

### Conversion Mode

Convertionには二つのモードが用意されています。

| Conversion Mode | 説明 |
| - | - |
| Convert And Destroy | GameObjectはConvert時にDestroyされます。これは後述する`IComponentConverter`を使用するために用いられます。 |
| Sync With Entity | 単一のEntityを生成してGameObjectと紐付け、GameObjectがDestroyされた際にEntityを削除します。また、Convert Hybrid Componentsにチェックを入れることでGameObjectの持つComponentを直接Entityに追加できます。 |

### EntityConversion.DefaultWorld

EntityConverterが生成するEntityは`EntityConversion.DefaultWorld`のWorldに追加されます。`EntityConversion.DefaultWorld`は起動時に自動で作成され、アプリケーションと同期して自動でDisposeされます。

### Component Converter

`IComponentConverter`を実装したMonoBehaviourはConverterとみなされ、Convert時に独自の処理を行うComponentとして動作するようになります。(`IComponentConverter`が実装されていない場合はHybrid Componentとして扱われます。)

以下は`IComponentConverter`の実装を行うサンプルです。

```cs
using UnityEngine;
using Arch.Unity.Conversion;

public class ExampleConverter : MonoBehaviour, IComponentConverter
{
    [SerializeField] float value;

    public void Convert(IEntityConverter converter)
    {
        converter.AddComponent(new ExampleComponent()
        {
            Value = value
        });
    }
}

public struct ExampleComponent
{
    public float Value;
}
```

### GameObjectReference

Conversion Modeが`Sync With Entity`の場合、対象のEntityには`GameObjectReference`コンポーネントが追加されます。これを介して同期されているGameObjectを取得することが可能です。

### パフォーマンス上の注意

`EntityConverter`はUnityのSubsceneとは異なり、実行時にEntityへの変換を行います。そのため、パフォーマンスが重要な場面やGameObjectとの同期が必要ない場面では可能な限り利用を避けることが推奨されます。

## C# Job Systemとの統合

Archには並列処理を行う独自のJobSchedulerが使用されていますが、Unityではより安全かつ高速なC# Job Systemが使用できます。Arch.Unityでは独自のJobインターフェースを実装することで、ArchのクエリをJob Systemで安全に並列処理することが可能になります。

また、これらのJobはBurst Compilerと互換性があります。Burstを適用することにより列挙を極めて高速に行うことが可能になります。

### IJobArchChunk

`IJobArchChunk`はChunk単位の処理を行うJobを作成するためのインターフェースです。Jobを作成するには、まずこれを実装した構造体を定義します。

```cs
using Unity.Burst;
using Arch.Unity.Jobs;

[BurstCompile]
public struct ExampleJob : IJobArchChunk
{
    public int ComponentId;

    public void Execute(NativeChunk chunk)
    {
        var array = chunk.GetNativeArray<ExampleComponent>(ComponentId);
        for (int i = 0; i < array.Length; i++)
        {
            var test = array[i];
            test.Value++;
            array[i] = test;
        }
    }
}

public struct ExampleComponent
{
    public float Value;
}
```

`NativeChunk`の`GetNativeArray<T>(int id)`を呼び出すことで、Componentのデータを指すNativeArrayを取得できます。ただし、取得にはComponent固有のIdを事前にJobに渡しておく必要があります。これは`Arch.Core.Utils.Component<T>.ComponentType.Id`などから取得が可能です。

```cs
var world = World.Create();
var query = new QueryDescription().WithAll<ExampleComponent>();

var job = new ExampleJob()
{
    ComponentId = Component<ExampleComponent>.ComponentType.Id
};
job.ScheduleParallel(world, query).Complete();
```

### 制限事項

HPC#の制約により参照型は使用できません。また、EntityやComponentの追加/削除など、構造的変化を伴う操作は一切行えないことに注意してください。(ArchのCommandBufferはHPC#と互換性がなく、現在Jobで使用可能な専用のCommandBufferはArch.Unityでは提供されていません。)

## エディタ拡張

### Arch Hierarchy

Arch.UnityではWorldごとのEntityを表示するEditorWindowを提供しています。これは`Window > Arch > Arch Hierarchy`から開くことができます。

<img src="https://github.com/AnnulusGames/Arch.Unity/blob/main/docs/images/img-arch-hierarchy.png" width="600">

Entityの名前にある数字は`(Index:Version)`を表します。

### Inspector

Arch HierarchyでEntityを選択すると、Inspector上にそのEntityが持つComponentの一覧を表示します。これらは読み取り専用ですが、値の変更がリアルタイムで反映されるためデバッグに便利です。

また、Entityが`GameObjectReference`コンポーネントを持つ場合には同期されているGameObjectが表示されます。

<img src="https://github.com/AnnulusGames/Arch.Unity/blob/main/docs/images/img-inspector.png" width="600">

## Toolkit

Arch自体はSystemを提供しておらず、[Arch.Extended](https://github.com/genaray/Arch.Extended)でSystem用のAPIおよびSource Generatorが提供されています。これらは非常に便利ですが、Unityで利用する上ではあまり扱いやすいとは言えません。Arch.UnityではArch.SystemをUnityで扱いやすくするための独自の機能を提供します。

### UnitySystemBase

Arch.Unityでは`BaseSystem<W, T>`の代わりに`UnityBaseSystem`を継承してSystemを実装します。`UnityBaseSystem`は`BaseSystem<World, SystemState>`を継承しており、`SystemState`からは`Time`や`DeltaTime`などの情報を取得できます。

```cs
public class FooSystem : UnitySystemBase
{
    public FooSystem(World world) : base(world) { }

    public override void Update(in SystemState state)
    {
        
    }
}
```

### ArchApp

`ArchApp`はArch.Unityで提供されるクラスで、UnityのPlayerLoop上でWorldとSystemを統合的に扱う機能を提供します。(ArchAppの設計はBevy EngineのAppにインスパイアされています。)

ArchAppを作成するには`ArchApp.Create()`を使用します。外部から使用するWorldを渡すことも可能ですが、指定がない場合は自動で新しいWorldを作成します。

```cs
var app = ArchApp.Create();
```

作成したAppにはSystemを追加できます。追加したSystemは自動的にPlayerLoop上にスケジュールされます。(後述するSystemRunnerを指定することで実行タイミングを設定することも可能です。)

```cs
app.AddSystems(systems =>
{
    systems.Add<FooSystem>();
});

app.AddSystems(SystemRunner.FixedUpdate, systems =>
{
    systems.Add<BarSystem>();
});
```

最後に`Run()`を呼び出すことでArchAppは動作を開始します。停止したい場合には`Stop()`を呼び出します。

```cs
app.Run();
```

これらはメソッドチェーンを用いて記述することも可能です。

```cs
ArchApp.Create()
    .AddSystems(systems =>
    {
        systems.Add<FooSystem>();
    })
    .AddSystems(SystemRunner.FixedUpdate, systems =>
    {
        systems.Add<BarSystem>();
    })
    .Run();
```

また、使用後は`Dispose()`で破棄する必要があります。この時内部のWorldも同時に破棄されます。

```cs
app.Dispose();
```

### SystemRunner

SystemRunnerはSystemの駆動を抽象化する機能です。Arch.UnityではPlayerLoop上で動作するいくつかのSystemRunnerと、単体テスト用の`FakeSystemRunner`が用意されています。

また、`ISystemRunner`を実装して独自のSystemRunnerを作成することも可能です。

```cs
public interface ISystemRunner
{
    void Run();
    void Add(ISystem<SystemState> system);
    void Remove(ISystem<SystemState> system);
}
```

## VContainer Integration

Arch.Unityでは`ArchApp`を[VContainer](https://github.com/hadashiA/VContainer)で扱うための拡張が用意されています。

```cs
using Arch.Unity;
using Arch.Unity.Conversion;
using VContainer;
using VContainer.Unity;

public class ExampleLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // コンテナの構築時に新しいArchAppを作成します
        // 作成したArchAppとWorld、追加したSystemが自動的にDIコンテナに登録されます
        builder.UseNewArchApp(Lifetime.Scoped, EntityConversion.DefaultWorld, systems =>
        {
            systems.Add<FooSystem>();
            systems.Add<BarSystem>();
        });
    }
}
```

## ライセンス

[MIT License](LICENSE)
# Naming Conventions

Types and methods are generally named according to the Microsoft C# standard naming conventions: 
[Identifier names - rules and conventions - C# | Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/identifier-names)

Here is a list of domain-specific naming patterns used throughout this project:

- Data components with no fields end with the word "Tag"
    ```c#
    public struct CooldownTag : IComponentData {}
    ```

- Enableable components with no fields end with the word "Flag"
    ```c#
    public struct InitFlag : IComponentData, IEnableableComponent {}
    ```

- All job structs end with the word "Job"
    ```c#
    public struct SomeJob : IJobEntity {}
    ```

- All systems end with the word "System"
    ```c#
    public partial struct SomeSystem : ISystem {}
    public partial class SomeSystem : SystemBase {}
    ```

- Authoring scripts end with the word "Authoring" and contain a private class called "Baker"
    ```c#
    public class SomeAuthoring : MonoBehaviour
    {
        // Public authoring fields go here
  
        private class Baker : Baker<SomeAuthoring>
        {
            public override void Bake(SomeAuthoring authoring)
            {
                // Baking code goes here
            }
        }
    }
  
    //ECS related components go here
    public struct SomeTag : IComponentData {}
    ```

- ScriptableObjects end with the word "Properties"
    ```c#
    public class SettingsProperties : ScriptableObject {}
    ```
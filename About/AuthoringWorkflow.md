# Authoring workflow

This package provides flexible authoring components that can serve your purposes.

### Regular Authorings

The simplest way to use NSprites is to use ready monobehaviour authoring components from this package.
These authoring components give you all of the data assignments you need in order to render sprites within the pipeline of this package.

- [`SpriteRendererAuthoring`](../Base/Authoring/SpriteRendererAuthoring.cs) - bakes default render components + adds sorting components.
- [`SpriteAnimatedRendererAuthoring`](../Animation/Authoring/SpriteAnimatedRendererAuthoring.cs) - same as previous, but also adds animation related components. Though not inherited from previous one (because it hard to keep all in place when you deal with unity serialization)

### Authoring Modules

Depending on your use case, you may want to implement your own registration system, your own shader, or some other components.
In that case, you may want to only use part of the authoring functionality.
To allow this, all of the authoring functionality is broken up into **Authoring Modules**, which are just serialized types with a `Bake()` method.

For example, if you only want to set up the sorting components, you may want to only use the authoring functionality specifically for sorting.
To do this, you can use the [`SortingAuthoringModule`](/Sorting/Authoring/Modules/SortingAuthoringModule.cs)
as well as whatever other modules you want, as fields in your custom authoring `MonoBehaviour` class. Then, you simply call `Bake()` in `Baker<TAuthoring>`:

```csharp
public class FooAuthoring : MonoBehaviour
{
    private class Baker : Baker<FooAuthoring>
    {
        public void override Bake(FooAuthoring authoring)
        {
            authoring.Sorting.Bake(this);
        }
    }

    public SortingAuthoringModule Sorting;
}
```

### Baker Extensions

If the authoring modules don't fit your needs, or they have extra unnecessary data, then you can
still use **Baker Extensions** which are used by the authoring modules.

For example, if you still want to use sorting but you don't need to set any sorting data because it is constant, then you can do something like this:

```csharp
public class FooAuthoring : MonoBehaviour
{
    private class Baker : Baker<FooAuthoring>
    {
        public void override Bake(FooAuthoring authoring)
        {
            this.BakeSpriteSorting
            (
                GetEntity(TransformUsageFlags.None),
                SortingIndex,
                SortingLayer,
                UseStaticSorting
            );
        }
    }

    private const int SortingIndex = 0;
    private const int SortingLayer = 0;
    private const bool UseStaticSorting = false;
}
```

### Relationship Diagram

The below diagram shows how the different parts of the authoring workflow relate to each other.

<img src="NSprites-Foundation-Authoring.drawio.svg" width="800"/>

# Assets Used for Authoring Workflow

- [`PropertiesSet`](/Base/Data/PropertiesSet.cs) - A `ScriptableObject` that contains rendering component names, with their update strategy types (read more about [properties](https://github.com/Antoshidza/NSprites/wiki/Register-components-as-properties) and [update modes](https://github.com/Antoshidza/NSprites/wiki/Property-update-modes)).
  This is used by **authoring** / **modules** / **extensions** to bake registration data. You can create it by calling context menu in project `Create/NSprites/Properties Set`.
- [`SpriteAnimation`](/Animation/Data/SpriteAnimation.cs) & [`SpriteAnimationSet`](/Animation/Data/SpriteAnimationSet.cs) - A pair of `ScriptableObject` types. The first contains animation data for a single animation, and second contains a set of `SpriteAnimation` objects.

# Sorting
Provides components / systems to perform 2D sorting based on [2D transforms](2DTransform.md) position relatively to camera view. As a result of sorting we have component [`SortingValue`](https://github.com/Antoshidza/NSprites-Foundation/blob/main/Base/Components/Properties/SpriteSortingValue.cs) which has [0..1] float value. Higher the value - sprite closer to camera.

## Regular sorting
Just simple screen view sorting. Compares screen position / sorting index and writes result. Performed every frame.

## Static sorting
Works like regular one but instead of sorting each frame sort sprites ones and every time when order version changes (which means there was entity creation / destruction). This sorting supposed to use for static sprites which won't be moved anytime. For example for backgroud objects like grass / stones.
> Note: sprites with different sorting type can't exist on the same layer. Well... it can, but you can face unexpected results.

## Layer
Sprite sorted between each other only on the same layer. Use different layers for sprites which you don't want to sort between each other. For example background mountain will never be sorted with characters.
There is constantly defined 8 layers, but there is also no limit, you can redefine this number to anything you want.
Higher layer - sprite closer to camera.

## Sorting Index
Sorting index has priority on position in terms of sorting. Higher sorting index - sprite closer to camera. If sprites have same sorting index then they will be sorted depending on screen position.
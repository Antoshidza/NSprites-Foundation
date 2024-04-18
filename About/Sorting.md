# Sorting
Provides components / systems to perform 2D sorting based on [2D transforms](2DTransform.md) position relatively to camera view.
Sorting happens inside shader, the only data passed to shader is `SortingData` with `int2` inside, where `x` is layer index and `y` is sorting index.

Shader uses technique where `SV_Position.z` changes to make sprites visually by sorted depending on:
* viewport y position
* layer index
* sorting index

This is achieved by splitting `[0..1]` range on smaller ranges, where:
* `per-layer-offset` = `1f` / `SpriteSortingSystem.LayerCount`
* `per-sorting-index-offset` = `per-layer-offset` / `SpriteSortingIndex.SortingIndexCount`
* `viewport-y-offset` = `per-sorting-index-offset` * `viewport-y`

## Sorting Index
Sorting index has priority on position in terms of sorting. Higher sorting index - sprite closer to camera. If sprites have same sorting index then they will be sorted depending on screen position.

## Layer
Basically layer is just the same as sorting index but one level above. 
It works completely the same way. 
It is implemented just because it is hard to organize draw order only using sorting indexes.
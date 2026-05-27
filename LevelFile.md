# Level File Format

First Line: `chapter`:`level`:`name`

Second Line: `width`x`height`

Third Line: `moveThresh1`:`moveThresh2`

After that: A list of statements for element placement

## Element Syntaxes

*`[]` marks optional arguments

+ Drain: `Drain (x, y) (C, M, Y)` (position, colour)
+ Droplet: `Droplet (x, y) (C, M, Y)` (position, colour)
+ Wall (Vertical): `WallV (x, y)` (position of cell left to it)
+ Wall (Horizontal): `WallH (x, y)` (position of cell on top of it)
+ Sponge: `Sponge (x, y) L|R|U|D` (position, facing)
+ Block: `Block (x, y)` (position)
+ Salt: `Salt (x, y)` (position)
+ Portal: `Portal V|H (x1, y1) V|H (x2, y2) (colour)` (orientation, position) x2

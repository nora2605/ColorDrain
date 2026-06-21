using ColorDrain.Maths;
using System;
using System.Collections.Generic;
using System.Text;

namespace ColorDrain.Game;

internal record Wall(Coord Position, bool Vertical) : Element
{
}

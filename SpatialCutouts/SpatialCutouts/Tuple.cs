﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpatialCutouts
{
    class Tuple<T1, T2>
    {
        public T1 Item1;
        public T2 Item2;

        public Tuple(T1 one, T2 two)
        {
            Item1 = one;
            Item2 = two;
        }
    }
}

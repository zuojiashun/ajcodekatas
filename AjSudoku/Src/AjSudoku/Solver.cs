﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AjSudoku
{
    public class Solver
    {
        public CellInfo Resolve(Position position)
        {
            CellInfo ci = null;

            for (int number = 1; number <= position.Size; number++)
            {
                ci = Resolve(number, position);

                if (ci != null)
                    return ci;
            }

            int n;

            for (int x = 0; x < position.Size; x++)
                for (int y = 0; y < position.Size; y++)
                    if ((n = position.GetUniqueNumberAt(x, y)) > 0)
                        return new CellInfo() { number = n, x = x, y = y };

            return null;
        }

        public CellInfo Resolve(int number, Position position)
        {
            for (int x = 0; x < position.Size; x++)
            {
                int npossible = 0;
                int ny = 0;

                for (int y = 0; y < position.Size; y++)
                    if (position.CanPutNumberAt(number, x, y))
                    {
                        npossible++;
                        ny = y;
                    }

                if (npossible == 1)
                    return new CellInfo() { number = number, x = x, y = ny };
            }

            for (int y = 0; y < position.Size; y++)
            {
                int npossible = 0;
                int nx = 0;

                for (int x = 0; x < position.Size; x++)
                    if (position.CanPutNumberAt(number, x, y))
                    {
                        npossible++;
                        nx = x;
                    }

                if (npossible == 1)
                    return new CellInfo() { number = number, x = nx, y = y };
            }

            for (int ix = 0; ix < position.Size; ix += position.Range)
                for (int iy = 0; iy < position.Size; iy += position.Range)
                {
                    int npossible = 0;
                    int nx = 0;
                    int ny = 0;

                    for (int x = 0; x < position.Range; x++)
                        for (int y = 0; y < position.Range; y++)
                        {
                            if (position.CanPutNumberAt(number, ix + x, iy + y))
                            {
                                npossible++;
                                nx = ix + x;
                                ny = iy + y;
                            }
                        }

                    if (npossible == 1)
                        return new CellInfo() { number = number, x = nx, y = ny };
                }

            return null;
        }
    }
}
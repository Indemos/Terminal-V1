/* Copyright (C) 2019 Interactive Brokers LLC. All rights reserved. This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */

using System;

namespace IBApi
{
    internal class BitMask
    {
        private int m_mask;

        public BitMask(int p) => m_mask = p;

        public int GetMask() => m_mask;

        public void Clear() => m_mask = 0;

        public bool this[int index]
        {
            get
            {
                if (index >= 32)
                {
                    throw new IndexOutOfRangeException();
                }

                return (m_mask & (1 << index)) != 0;
            }
            set
            {
                if (index >= 32)
                {
                    throw new IndexOutOfRangeException();
                }

                if (value)
                {
                    m_mask |= 1 << index;
                }
                else
                {
                    m_mask &= ~(1 << index);
                }
            }
        }
    }
}

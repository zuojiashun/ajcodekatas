﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ItemsTablePerHierarchyMapping.Domain
{
    public class Page : Item
    {
        public virtual string Url { get; set; }
    }
}

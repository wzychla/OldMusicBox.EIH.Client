﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Xml;
using System.IO;
using System.Text;
using System.Collections;

namespace Org.BouncyCastle.Crypto.Xml
{
    // the interface to be implemented by all subclasses of XmlNode
    // that have to provide node subsetting and canonicalization features.
    internal interface ICanonicalizableNode
    {
        bool IsInNodeSet
        {
            get;
            set;
        }

        void Write(StringBuilder strBuilder, DocPosition docPos, AncestralNamespaceContextManager anc);
        void WriteHash(IHash signer, DocPosition docPos, AncestralNamespaceContextManager anc);
    }
}

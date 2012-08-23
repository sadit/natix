#!/usr/bin/env python
#
#   Copyright 2012 Eric Sadit Tellez <sadit@dep.fie.umich.mx>
#
#   Licensed under the Apache License, Version 2.0 (the "License");
#   you may not use this file except in compliance with the License.
#   You may obtain a copy of the License at
#
#       http:#www.apache.org/licenses/LICENSE-2.0
#
#   Unless required by applicable law or agreed to in writing, software
#   distributed under the License is distributed on an "AS IS" BASIS,
#   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
#   See the License for the specific language governing permissions and
#   limitations under the License.
#
#   Original filename: natix/natix/Numeric/replace-templates.py
# 
import types

for srcname in ("Numeric",):
    vs = file("%s.cs.template"%srcname).read()
    for xtype in ("Char", "Byte", "SByte", "Single", "Double", "UInt16", "UInt32", "Int16", "Int32", "Int64", "UInt64"):
        if xtype[1] in ('I'):
            name = "UInt" + xtype[-2:]
        elif xtype == 'SByte':
	    name = xtype
        else:
            name = xtype.title()
        fname = "%s%s.cs"%(srcname,name)
        print fname
        f = file(fname, "w")
        f.write(vs.replace("TYPENAME", name).replace("TYPE", xtype))
        f.close()

﻿
open database TestDb connectionstring "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=.;Extended Properties=dBASE IV;User ID=Admin;Password=;" provider "System.Data.OleDb"

use Test command "select * from TEST order by CODIGO"

move := Test.MoveNext()
code := Test.GetField("CODIGO")





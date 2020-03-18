The file "ObjectBuilder.cs" has been modified on line 616
--> old : string shaderName = "Standard";// (md.illumType == 2) ? "Standard (Specular setup)" : "Standard";
--> new : string shaderName = (md.illumType == 2) ? "Standard (Specular setup)" : "Standard";
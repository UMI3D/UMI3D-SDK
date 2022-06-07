This unitypackage containt the edk package and an empty server scene

Some unity error could occured

Your project should use .Net 4.x :
	player settings -> other settings -> Configuration -> Api Compatibility level : .net 4.x

Error with newtonsoft:
	Newtonsoft is used by many library. It migth already be used in your project or a unity library.
	first set "player settings -> other settings -> Configuration -> Assembly Version Validation" to false
	Then look for newtonsoft dll in the edk and exclude all plateform.
	
Error with undeterministic version :
	deterministic build player settings -> other settings -> Script compilation -> Use deterministic compilation : false
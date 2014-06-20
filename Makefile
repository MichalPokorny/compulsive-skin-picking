csps.exe: *.cs Tests/*.cs
	mcs *.cs Tests/*.cs -out:csps.exe -debug

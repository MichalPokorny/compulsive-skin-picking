csps.exe: *.cs */*.cs
	mcs *.cs */*.cs -out:csps.exe -debug

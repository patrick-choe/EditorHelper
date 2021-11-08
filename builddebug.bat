dotnet "C:\Program Files\dotnet\sdk\5.0.400\MSBuild.dll" /p:Configuration=Debug
mkdir Debug
cd ./Debug
mkdir EditorHelper
cp ../EditorHelper/bin/Debug/EditorHelper.dll ./EditorHelper/EditorHelper.dll
cp ../EditorHelper/bin/Debug/MathParser.org-mXparser.dll ./EditorHelper/MathParser.org-mXparser.dll

cp ../dlls/*.dll ./EditorHelper/
cp ../Unity/EditorHelper/EditorHelper/Assets/AssetBundles/editorhelper ./EditorHelper/
cp ../Info.json ./EditorHelper/Info.json
cp ../Version.txt ./EditorHelper/Version.txt
tar -acf EditorHelper-Debug.zip EditorHelper
mv EditorHelper-Debug.zip ../
cd ../
rm -rf Debug
pause

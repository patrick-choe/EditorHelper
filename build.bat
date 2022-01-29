dotnet "C:\Program Files\dotnet\sdk\5.0.400\MSBuild.dll" /p:Configuration=Release
mkdir Release
cd ./Release
mkdir EditorHelper
cp ../EditorHelper/bin/Release/EditorHelper.dll ./EditorHelper/EditorHelper.dll
cp ../dlls/* ./EditorHelper/
cp ../Unity/EditorHelper/EditorHelper/Assets/AssetBundles/editorhelper ./EditorHelper/
cp ../Info.json ./EditorHelper/Info.json
cp ../Version.txt ./EditorHelper/Version.txt
tar -acf EditorHelper-Legacy.zip EditorHelper
mv EditorHelper-Legacy.zip ../
cd ../
rm -rf Release
pause

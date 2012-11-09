# AimingLib 以下のコードを Assets/Aiming にコピー
# Test から始まるフォルダーと、Sample から始まるフォルダーは除外
# .cs ファイルと .meta ファイル以外は削除

$testProject = 'Test*'
$sampleProject = 'Sample*'
$excludeFolders = 'Properties', 'bin', 'obj'

$from = '.'
$to = '..\Assets\Aiming'

$projects = ls |
    ?{ -not ($_.Name -like $testProject) } |
    ?{ -not ($_.Name -like $sampleProject) } |
    ?{ $_ -is [IO.DirectoryInfo] }

# いったん丸ごとコピー
foreach($p in $projects)
{
    copy $p.FullName $to -Force -Recurse
}

# bin とかのフォルダーはじき
foreach($ex in $excludeFolders)
{
    ls $to -Recurse | ?{ $_.Name -eq $ex } | ?{ $_ -is [IO.DirectoryInfo] } | rm -Recurse
}

# .cs 以外不要なので削除
ls $to -Recurse -Force | ?{ ($_.Extension -ne '.cs') -and ($_.Extension -ne '.meta') } | ?{ $_ -is [IO.FileInfo] } | rm -Force

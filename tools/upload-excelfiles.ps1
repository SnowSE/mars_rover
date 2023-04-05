$files = gci "../src/Mars.Web/data/hidden/*.xlsx"
$files | %{
    $src = $_.FullName
    $dest = "data/hidden/$($_.Name)"
    write-host "Copying $src to $dest"
    az webapp deploy --resource-group mars_rover --name snow-rover --src-path $src --type static --target-path $dest
}
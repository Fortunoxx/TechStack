# this script finds out the exact tags from docker hub by digest id
# docker images --digests
# $repository = "grafana/grafana"  # Replace with your repository name
# $digestId = "sha256:b23b588cf7cba025ec95efba82e0d8d2e5d549a8b2cb5d50332d4175693c54e0"  # Replace with your digest ID
param($repository, $digestId)

if (!$repository) {
    Write-Host "Missing parameter repository"
    exit
}

if (!$digestId) {
    Write-Host "Missing parameter digestId"
    exit
}

if (!($repository -like "*/*")) {
    $repository = "library/" + $repository # this seems to work for mongo, redis, postgres...
    Write-Host "repository is now " $repository
}

# Function to get all tags for a given repository
function Get-DockerTags {
    param (
        [string]$Repository
    )

    $tags = @()
    $page = 1
    $pageSize = 100
    $hasMore = $true
    $quitAfterFirstOtherThanLatest = $false

    while ($hasMore) {
        $url = "https://hub.docker.com/v2/repositories/$Repository/tags/?page=$page&page_size=$pageSize"
        $response = Invoke-RestMethod -Uri $url

        foreach ($tag in $response.results) {
            if ($tag.digest -eq $digestId) {
                Write-Host -ForegroundColor Cyan "Found tag: $($tag.Name) for digest: $digestId - continuing search..."

                $tagInfo = [PSCustomObject]@{
                    Name = $tag.name
                    Digest = $tag.digest
                }
                $tags += $tagInfo
            }

            ## there are digests in images, but they don't match the docker digest-hashes
            # foreach ($image in $tag.images) {
            #     $tagInfo = [PSCustomObject]@{
            #         Name = $tag.name
            #         Digest = $image.digest
            #     }
            #     Write-Host -ForegroundColor Cyan $image.digest
            #     $tags += $tagInfo
            # }
        }

        $page++
        $hasMore = $response.next -ne $null
    }

    return $tags
}

# Fetch tags
$tags = Get-DockerTags -Repository $repository

# Check each tag's digest
$found = $false
foreach ($tag in $tags) {
    if ($tag.Digest -eq $digestId) {
        Write-Host -ForegroundColor Green "Found tag: $($tag.Name) for digest: $digestId"
        $found = $true
        # break
    }
}

if (-not $found) {
    Write-Host -ForegroundColor Yellow "No tag found for digest: $digestId"
}

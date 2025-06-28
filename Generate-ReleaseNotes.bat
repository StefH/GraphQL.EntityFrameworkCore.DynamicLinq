rem https://github.com/StefH/GitHubReleaseNotes

SET version=1.0.3

GitHubReleaseNotes --output CHANGELOG.md --skip-empty-releases --exclude-labels wontfix dependencies question invalid documentation --version %version% --token %GH_TOKEN%
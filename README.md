# Arcane Gambit
Arcane gambit is a 2d roguelite game made using the Unity 6 Engine.

# Workflows
This project use git LFS to track large binary files, and as such any contributor that want to work on this project should have git LFS installed on their system.

## Linux Installation
See the corresponding guide in the official repo [here](https://github.com/git-lfs/git-lfs/blob/main/INSTALLING.md)

## Windows Installation
1. Download the git LFS executable from [here](https://git-lfs.com/)
    - Or if you prefer winget you can use it by running this command `winget install -e --id GitHub.GitLFS`
2. Double click the `.exe` file to install it
3. Enter the command `git lfs install` to install the LFS plugin.

Git LFS Settings (Which file should be added to LFS) is already preconfigured here, but if you want to add more, you should read the official git LFS guide [here](https://git-lfs.com/).

## Pulling Changes
In every pull, other than the `git pull <remote` command, it should be followed by `git lfs pull`.
:: @name Stash And Move To develop
:: @description Stash all changes, move to develop, and apply changes

git stash -u
git checkout develop
git -c diff.mnemonicprefix=false -c core.quotepath=false fetch --prune origin
git pull --rebase
git stash pop

:: @name Discard Changes & Move To develop
:: @description Discard all local changes, move to and update develop

git stash save -u "delete"
git stash drop stash@{0}
git checkout develop
git -c diff.mnemonicprefix=false -c core.quotepath=false fetch --prune origin
git pull --rebase

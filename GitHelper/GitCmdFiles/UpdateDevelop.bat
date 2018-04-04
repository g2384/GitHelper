:: @name Update develop
:: @description checkout develop and update develop

git checkout develop
git -c diff.mnemonicprefix=false -c core.quotepath=false fetch --prune origin
git pull --rebase

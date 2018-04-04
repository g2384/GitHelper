:: @name Update Current Branch
:: @description fetch origin, pull (with rebase)

git -c diff.mnemonicprefix=false -c core.quotepath=false fetch --prune origin
git -c diff.mnemonicprefix=false -c core.quotepath=false pull --no-commit --rebase origin

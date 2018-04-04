:: @name Show Remote Branches
:: @description Show authers of all branches (remote and local) branches, ordered by most recent commit date

git for-each-ref --format='%(committerdate) %09 %(authorname) %09 %(refname)' --sort=committerdate

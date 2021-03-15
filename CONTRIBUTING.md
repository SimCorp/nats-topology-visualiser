# Contribution and Git Workflow Guidelines

## Branches
When starting work on a new task, create a new branch from the *develop* branch. Make sure to fetch before doing this, so that you have the newest version. Name branches with a category and a short descriptive name, in lower-case and hyphenated, like this:
> feature/my-cool-feature

Try to use one of these categories
- `feature` — A feature branch
- `hotfix` — Urgent fixes for production code issues
- `bug` — A bugfix branch
- `chore` — Adding documentation, cleaning up and organizing code. (Adding unit tests, should be included in features)
- `exp` — Experimental work that probably won't get production ready directly 

A new branch can be made using the command line like this
>git checkout -b name-of-the-branch

Or through GitHub desktop by clicking *Branch* and then *new branch*

## Commit messages
These should be formatted like proposed by [Chris Beams](https://chris.beams.io/posts/git-commit/).
Most importantly, keep the messages short, precise, and use imperative mood (e.g. "Refactor X for readability" or "Fix crash when ...")

You can use the sentence *"When applied, this commit will..."* to help formulate the commit message.

> *When applied, this commit will*, **Refactor subsystem X for readability**

> *When applied, this commit will*, **Update getting started documentation**

## Pull request
When you believe that your work is done and ready to be used, open a pull request by going to the repository on GitHub → *Pull requests* → *New pull request*

Set the base to *develop* and the one to be compared to, to *feature/your-branch*. Then request a review from those who you would like to review the changes.

Tests will be run on the code automatically through GitHub Actions.

Once your pull request has been manually approved and passes all other checks, you can merge it, preferably by choosing the option *Rebase and Merge*.

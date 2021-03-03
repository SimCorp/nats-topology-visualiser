# Git Guidelines

## Branches
When starting work on a new task, create a new branch from the *develop* branch. Make sure to fetch before doing this, so that you have the newest version. Name this branch like 
> feature/my-cool-feature

If you are working on a feature, or like this if you are working to fix a bug
>bug/description-of-the-issue

A new branch can be made using the command line like this
>git checkout -b name-of-the-branch

Or through GitHub desktop by clicking *Branch* and then *new branch*

## Commit messages
These should be formatted like proposed by [Chris Beams](https://chris.beams.io/posts/git-commit/).
Most importantly, keep the messages short, precise, and use imperative mood (fx. "Add page header" or "Fix crash when ...")

## Pull request
When you believe that your work is done and ready to be used, open a pull request by going to the repository on GitHub -> *Pull requests* -> *New pull request*
Set it such that the base is *develop* and the one to be compared to is *your-branch-name*. Choose at least one person that you would like to to review these changes, but multiple reviwers is great.
Once your pull request has been approved and passes all other checks, you can merge it, preferably by choosing the rebase-option.
BoltOn uses [GitHub Actions](https://github.com/features/actions) for CI and CD. 

CI involves just building and testing the projects, whereas CD involves building, testing, versioning, packing and publishing to NuGet, and tagging. Though pretty much most of them could be achieved using the GitHub tasks, we are using PowerShell to mainly support performing all the tasks locally. Both the PS scripts ([build.ps1](https://github.com/gokulm/BoltOn/blob/master/build/build.ps1) and [publish.ps1](https://github.com/gokulm/BoltOn/blob/master/build/publish.ps1)) use some of the functions in [bolton.psm1](https://github.com/gokulm/BoltOn/blob/master/build/bolton.psm1).

Versioning is done based on [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/). Here it is:

* feat: Increments minor version
* fix: Increments patch version
* feat! and fix!: Increments major version

[publish.ps1](https://github.com/gokulm/BoltOn/blob/master/build/publish.ps1) script finds all the changed projects in a merge, and increments the version(s) of them based on the commit message.

To force changing the versions of projects without making any changes to the project files, scope could be specified in the commit message:

Example:

feat(BoltOn, BoltOn.Data.EF): test comments

Which will increment minor versions of both BoltOn and BoltOn.Data.EF projects, and publish them.


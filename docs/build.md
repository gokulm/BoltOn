BoltOn uses [GitHub Actions](https://github.com/features/actions) for CI and CD. 

CI involves just building and testing the projects, whereas CD involves building, testing, versioning, packing and publishing to NuGet, and tagging. Though pretty much most of them could be achieved using the GitHub tasks, we are using PowerShell to mainly support performing all the tasks locally. Both the PS scripts use some of the functions in [bolton.psm1](https://github.com/gokulm/BoltOn/blob/master/build/bolton.psm1).

Versioning is done based on [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/).


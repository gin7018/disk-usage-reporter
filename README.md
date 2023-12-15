# disk-usage-reporter
tool designed to report on file and folder space usage in a system

- uses the .dot net framework to build a simple disk usage reporter tool
- has the functionality to be run in parallel or single-threaded mode (or both)

### usage
```
Usage: du [-s] [-p] [-b] <path-to-analyse>
Summarize disk usage of the set of FILES, recursively for directories.

-s Run in single-threaded mode
-p Run in parallel mode (uses all available processors)
-b Run in both parallel and single-threaded mode. Runs parallel followed by sequential mode
```

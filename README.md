# FileSplitter
Various ways of splitting a file in C# using MEF
## Split Modes
- Shamir's Secret Sharing
- Confettis
- Normal slice (to be implemented)
## Projects role
- FileSplitterDef:      Interface definition for MEF, plus some reusable code
- FileSplitterImporter: Import the MEF assemblies
- FileSplitterLib:      This this where the split/merge ans read/write code is
- FileSplitterTests:    The tests
- ProvTest:             The old test project only for reference.
## Shamir's Secret Sharing
The files are splitted using Shamir's secret sharing algorithm.
## Confettis (or BitByBit)
The files are splitted on the bit level using the zipper merge/split principle.

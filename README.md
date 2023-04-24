# PSP_ELFHandle
A simple Command-Line Program for expanding an PlayStation Portable Executable (ELF)

The target should be an `EBOOT.BIN`, an executable from a PSP game. Desired size must be a multiple of `0x800`.

Source:
[Executable and Linkable Format](https://en.wikipedia.org/wiki/Executable_and_Linkable_Format)

# Usage
```shell
PSP_EBOOT_TOOLKIT <target> <output> <increase size>
```
# ELF Structure
Only the data to be modified will be listed below.

## ELF Header
| Offset | Type   | Name     | Description                                                |
|--------|--------|----------|------------------------------------------------------------|
| 0x1C   | Int32  | iPhoff   | Points to the start of the program header table            |
| 0x20   | Int32  | iShoff   | Points to the start of the section header table            |
| 0x2C   | UInt16 | iPhnum   | Contains the number of entries in the program header table |
| 0x2E   | UInt16 | iShSizeH | ontains the size of a section header table entry           |
| 0x30   | UInt16 | iShnum   | Contains the number of entries in the section header table |

## Header Entry
| Offset | Type  | Name    | Description                                              |
|--------|-------|---------|----------------------------------------------------------|
| 0x0    | Int32 | iType   | Identifies the type of the segment                       |
| 0x4    | Int32 | iOffset | Offset of the segment in the file image                  |
| 0x8    | Int32 | iVaddr  | Virtual address of the segment in memory                 |
| 0xC    | Int32 | iPaddr  | Reserved for segment's physical address                  |
| 0x10   | Int32 | iFilesz | Size in bytes of the segment in the file image           |
| 0x14   | Int32 | iMemsz  | Size in bytes of the segment in memory                   |
| 0x18   | Int32 | iFlags  | Segment-dependent flags                                  |
| 0x1C   | Int32 | iAlign  | Alignment of the segment, it should be an int power of 2 |

## Header Segment Header
| Offset | Type  | Name         | Description                                                                              |
|--------|-------|--------------|------------------------------------------------------------------------------------------|
| 0x0    | Int32 | iShname      | An offset to a string in the .shstrtab with the name of this section                     |
| 0x4    | Int32 | iShtype      | Identifies the type of this header                                                       |
| 0x8    | Int32 | iShflags     | Identifies the attributes of the section                                                 |
| 0xC    | Int32 | iShaddr      | Virtual address of the section in memory, for sections that are loaded                   |
| 0x10   | Int32 | iShoffset    | Offset of the section in the file image                                                  |
| 0x14   | Int32 | iShsize      | Size in bytes of the section in the file image                                           |
| 0x18   | Int32 | iShlink      | Contains the section index of an associated section                                      |
| 0x1C   | Int32 | iShinfo      | Contains extra information about the section                                             |
| 0x20   | Int32 | iShaddralign | Contains the required alignment of the section                                           |
| 0x24   | Int32 | iShentsize   | Contains the size, in bytes, of each entry, for sections that contain fixed-size entries |

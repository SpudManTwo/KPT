/*----------------------------------------------------------------------------*/
/*-- UMD-replace.c                                                          --*/
/*-- Tiny tool to replace data files in a PSP UMD ISO                       --*/
/*-- Copyright (C) 2012-2015 CUE                                            --*/
/*--                                                                        --*/
/*-- This program is free software: you can redistribute it and/or modify   --*/
/*-- it under the terms of the GNU General Public License as published by   --*/
/*-- the Free Software Foundation, either version 3 of the License, or      --*/
/*-- (at your option) any later version.                                    --*/
/*--                                                                        --*/
/*-- This program is distributed in the hope that it will be useful,        --*/
/*-- but WITHOUT ANY WARRANTY; without even the implied warranty of         --*/
/*-- MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the           --*/
/*-- GNU General Public License for more details.                           --*/
/*--                                                                        --*/
/*-- You should have received a copy of the GNU General Public License      --*/
/*-- along with this program. If not, see <http://www.gnu.org/licenses/>.   --*/
/*----------------------------------------------------------------------------*/

/*----------------------------------------------------------------------------*/
#include <stdio.h>
#include <stdlib.h>

/*----------------------------------------------------------------------------*/
typedef unsigned long long U64;

/*----------------------------------------------------------------------------*/
#define VERSION          "20150303"     // tool version

#define DESCRIPTOR_LBA   0x010          // LBA of the first volume descriptor
#define TOTAL_SECTORS    0x050          // total number of sectors in the UMD
#define ROOT_FOLDER_LBA  0x09E          // LBA of the root folder
#define ROOT_SIZE        0x0A6          // size of the root folder
#define TABLE_PATH_LEN   0x084          // size of the first table path
#define TABLE_PATH_LBA   0x08C          // LBA of the first table path

#define DESCRIPTOR_SIG_1 0x0001313030444301ULL // volumen descriptor signatures
#define DESCRIPTOR_SIG_2 0x00013130304443FFULL

#define LEN_SECTOR_M0    0x800 // M0
#define LEN_DATA_M0      0x800 // M0
#define POS_DATA_M0      0x000 // M0

#define TMPNAME          "umd-replace.$" // temporal name
#define BLOCKSIZE        16384           // sectors to read/write at once

/*----------------------------------------------------------------------------*/
#define EXIT(text)       { printf(text); exit(EXIT_FAILURE); }

/*----------------------------------------------------------------------------*/
void  Title(void);
void  Usage(void);

U64   FileSize(char *filename);
char *Load(char *filename);
void  Save(char *filename, char *buffer, int length);
char *Read(char *filename, U64 position, int length);
void  Write(char *filename, U64 position, int length, char *buffer);
void  Create(char *filename);
char *Memory(int length, int size);
int   StrLen(char *data);
int   ChangeEndian(char *value);

void  Replace(char *isoname, char *oldname, char *newname);
U64   Search(char *isoname, char *filename, char *path, int lba, int len);
void  PathTable(char *isoname, int lba, int len, int lba_old, int diff, int sw);
void  TOC(char *isoname, int lba, int len, U64 found, int lba_old, int diff);
char *ReadSectors(char *isoname, int lba, int sectors);
void *WriteSectors(char *isoname, int lba, char *buffer, int sectors);

/*----------------------------------------------------------------------------*/
unsigned int mode;        // image mode
unsigned int sector_size; // sector size
unsigned int data_offset; // sector data start
unsigned int sector_data; // sector data length

/*----------------------------------------------------------------------------*/
int main(int argc, char **argv) {
  Title();

  if (argc != 4) Usage();

  Replace(argv[1], argv[2], argv[3]);

  printf("\nDone\n");

  exit(EXIT_SUCCESS);
}

/*----------------------------------------------------------------------------*/
void Title(void) {
  printf(
    "\n"
    "UMD-REPLACE version %s - Copyright (C) 2012-2015 CUE\n"
    "Tiny tool to replace data files in a PSP UMD ISO\n"
    "\n",
    VERSION
  );
}

/*----------------------------------------------------------------------------*/
void Usage(void) {
  EXIT(
    "Usage: UMD-REPLACE imagename filename newfile\n"
    "\n"
    "- 'imagename' is the name of the UMD image\n"
    "- 'filename' is the file in the UMD image with the data to be replaced\n"
    "- 'newfile' is the file with the new data\n"
    "\n"
    "* 'imagename' must be a valid UMD ISO image\n"
    "* 'filename' can use either the slash or backslash\n"
    "* 'newfile' can be different size as 'filename'\n"
  );
}

/*----------------------------------------------------------------------------*/
U64 FileSize(char *filename) {
  FILE *fp;
  U64   fs;

  if ((fp = fopen(filename, "rb")) == NULL) EXIT("File open error\n");
  fseeko64(fp, 0, SEEK_END);
  fs = ftello64(fp);
  if (fclose(fp) == EOF) EXIT("File close error\n");

  return(fs);
}

/*----------------------------------------------------------------------------*/
char *Load(char *filename) {
  return(Read(filename, 0, FileSize(filename)));
}

/*----------------------------------------------------------------------------*/
void Save(char *filename, char *buffer, int length) {
  Write(filename, 0, length, buffer);
}

/*----------------------------------------------------------------------------*/
char *Read(char *filename, U64 position, int length) {
  FILE *fp;
  char *fb;

  if (position + length > FileSize(filename)) EXIT("Read past the end\n");

  if ((fp = fopen(filename, "rb")) == NULL) EXIT("File open error\n");
  fb = Memory(length, sizeof(char));
  if (fseeko64(fp, position, SEEK_SET)) EXIT("File seek error\n");
  if (fread(fb, 1, length, fp) != length) EXIT("File read error\n");
  if (fclose(fp) == EOF) EXIT("File close error\n");

  return(fb);
}

/*----------------------------------------------------------------------------*/
void Write(char *filename, U64 position, int length, char *buffer) {
  FILE *fp;

  if ((fp = fopen(filename, "r+b")) == NULL) EXIT("File open error\n");
  if (fseeko64(fp, position, SEEK_SET)) EXIT("File seek error\n");
  if (fwrite(buffer, 1, length, fp) != length) EXIT("File write error\n");
  if (fclose(fp) == EOF) EXIT("File close error\n");
}

/*----------------------------------------------------------------------------*/
void Create(char *filename) {
  FILE *fp;

  if ((fp = fopen(filename, "w+b")) == NULL) EXIT("File create error\n");
  if (fclose(fp) == EOF) EXIT("File close error\n");
}

/*----------------------------------------------------------------------------*/
char *Memory(int length, int size) {
  char *fb;

  fb = (char *)calloc(length * size, size);
  if (fb == NULL) EXIT("Memory error\n");

  return(fb);
}

/*----------------------------------------------------------------------------*/
int StrLen(char *data) {
  unsigned int i;

  for (i = 0; data[i]; i++);

  return(i);
}

/*----------------------------------------------------------------------------*/
int ChangeEndian(char *value) {
  char new[4];

  new[0] = value[3];
  new[1] = value[2];
  new[2] = value[1];
  new[3] = value[0];

  return(*(int *)new);
}

/*----------------------------------------------------------------------------*/
void Replace(char *isoname, char *oldname, char *newname) {
  FILE          *fp_iso;
  unsigned char *buffer, *tmp, name[256];
  unsigned int   image_sectors, total_sectors, root_lba, root_length;
  U64            found_position;
  unsigned int   found_lba, found_offset;
  unsigned int   old_filesize, old_sectors, file_lba;
  unsigned int   new_filesize, new_sectors, new_length;
  unsigned int   diff, l_endian, b_endian, lba;
  unsigned int   tbl_lba, tbl_len;
  unsigned int   count, maxim;
  unsigned int   i, j, k;

  sector_size = LEN_SECTOR_M0;
  data_offset = POS_DATA_M0;
  sector_data = LEN_DATA_M0;

  // get data from the primary volume descriptor
  buffer = ReadSectors(isoname, DESCRIPTOR_LBA, 1);

  image_sectors = *(unsigned int *)(buffer + data_offset + TOTAL_SECTORS);
  total_sectors = FileSize(isoname) / sector_size;
  root_lba = *(unsigned int *)(buffer + data_offset + ROOT_FOLDER_LBA);
  root_length = *(unsigned int *)(buffer + data_offset + ROOT_SIZE);
  free(buffer);

  // get new data from the new file
  new_filesize = FileSize(newname);
  new_sectors = (new_filesize + sector_data - 1) / sector_data;

  // 'oldname' must start with a path separator
  if ((oldname[0] != '/') && (oldname[0] != '\\')) {
    i = StrLen(oldname) + 1;
    while (i--) oldname[i + 1] = oldname[i];
    oldname[0] = '/';
  }
  // change all backslashes by slashes in 'oldname'
  i = StrLen(oldname);
  while (i--) if (oldname[i] == '\\') oldname[i] = '/';

  // search 'oldname' in the image
  found_position = Search(isoname, oldname, "", root_lba, root_length);
  if (!found_position) EXIT("File not found in the UMD image\n");

  found_lba    = found_position / sector_size;
  found_offset = found_position % sector_size;

  // get data from the old file
  buffer = ReadSectors(isoname, found_lba, 1);

  old_filesize = *(unsigned int *)(buffer + found_offset + 0x0A);
  old_sectors = (old_filesize + sector_data - 1) / sector_data;
  file_lba = *(unsigned int *)(buffer + found_offset + 0x02);

  free(buffer);

  // size difference in sectors
  diff = new_sectors - old_sectors;

  // image name
  sprintf(name, "%s", old_sectors == new_sectors ? isoname : TMPNAME);

  if (diff) {
    // create the new image
    printf("- creating temporal image\n");

    Create(name);

    lba = 0;

    // update the previous sectors
    printf("- updating previous data sectors\n");

    maxim = file_lba;
    for (i = 0; i < file_lba; ) {
      count = maxim >= BLOCKSIZE ? BLOCKSIZE : maxim; maxim -= count;

      buffer = ReadSectors(isoname, i, count);
      WriteSectors(name, lba, buffer, count); lba += count;
      free(buffer);

      i += count;
    }
  } else {
    lba = file_lba;
  }

  // update the new file
  printf("- updating file data\n");

  if (new_sectors) {
    // read and update all data sectors except the latest one (maybe incomplete)
    maxim = --new_sectors;
    for (i = 0; i < new_sectors; ) {
      count = maxim >= BLOCKSIZE ? BLOCKSIZE : maxim; maxim -= count; 

      buffer = Memory(count * sector_size, sizeof(char));
      tmp = Read(newname, i * sector_data, count * sector_data);
      for (j = 0; j < count; j++) {
        for (k = 0; k < sector_data; k++) {
          buffer[j * sector_size + data_offset + k] = tmp[j * sector_data + k];
        }
      }
      WriteSectors(name, lba, buffer, count); lba += count;
      free(tmp);
      free(buffer);

      i += count;
    }
    new_sectors++;

    // read and update the remaining data sector
    new_length = new_filesize - i * sector_data;

    buffer = Memory(sector_size, sizeof(char));
    tmp = Read(newname, i * sector_data, new_length);
    for (j = 0; j < new_length; j++) buffer[data_offset + j] = tmp[j];
    WriteSectors(name, lba++, buffer, 1);
    free(tmp);
    free(buffer);
  }

  if (diff) {
    // update the next sectors
    printf("- updating next data sectors\n");

    maxim = total_sectors - (file_lba + old_sectors);
    for (i = file_lba + old_sectors; i < total_sectors; ) {
      count = maxim >= BLOCKSIZE ? BLOCKSIZE : maxim; maxim -= count;

      buffer = ReadSectors(isoname, i, count);
      WriteSectors(name, lba, buffer, count); lba += count;
      free(buffer);

      i += count;
    }
  }

  if (new_filesize != old_filesize) {
    // update the file size
    printf("- updating file size\n");

    l_endian = new_filesize;
    b_endian = ChangeEndian((char *)&l_endian);

    buffer = ReadSectors(name, found_lba, 1);
    *(unsigned int *)(buffer + found_offset + 0x0A) = l_endian;
    *(unsigned int *)(buffer + found_offset + 0x0E) = b_endian;
    WriteSectors(name, found_lba, buffer, 1);
    free(buffer);
  }

  if (diff) {
    // update the primary volume descriptor
    printf("- updating primary volume descriptor\n");

    l_endian = image_sectors + diff;
    b_endian = ChangeEndian((char *)&l_endian);

    buffer = ReadSectors(name, DESCRIPTOR_LBA, 1);
    *(unsigned int *)(buffer + data_offset + TOTAL_SECTORS) = l_endian;
    *(unsigned int *)(buffer + data_offset + TOTAL_SECTORS + 4) = b_endian;
    WriteSectors(name, DESCRIPTOR_LBA, buffer, 1);
    free(buffer);

    // update the path tables
    printf("- updating path tables\n");

    buffer = ReadSectors(name, DESCRIPTOR_LBA, 1);
    for (i = 0; i < 4; i++) {
      tbl_len = *(unsigned int *)(buffer + data_offset + TABLE_PATH_LEN);
      tbl_lba = *(unsigned int *)(buffer + data_offset + TABLE_PATH_LBA + 4*i);
      if (tbl_lba) {
        if (i & 0x2) tbl_lba = ChangeEndian((char *)&tbl_lba);
        PathTable(name, tbl_lba, tbl_len, file_lba, diff, i & 0x2);
      }
    }
    free(buffer);

    // update the file/folder LBAs
    printf("- updating entire TOCs\n");

    TOC(name, root_lba, root_length, found_position, file_lba, diff);

    // remove the old image
    printf("- removing old image\n");

    if (remove(isoname)) EXIT("Remove file error\n");

    // rename the new image
    printf("- renaming temporal image\n");

    if (rename(name, isoname)) EXIT("Rename file error\n");
  }

  printf("- the new image has ");
  if      ((int)diff > 0) printf("%d more", diff);
  else if ((int)diff < 0) printf("%d fewer", -diff);
  else                    printf("the same", diff);
  printf(" sector"); if (((int)diff != 1) && ((int)diff != -1)) printf("s");
  if (!diff) printf(" as"); else printf(" than");
  printf(" the original image\n");
  if (diff) {
    printf("- maybe you need to hand update the cuesheet file");
    printf(" (if exist and needed)\n");
  }
}

/*----------------------------------------------------------------------------*/
U64 Search(char *isoname, char *filename, char *path, int lba, int len) {
  unsigned char *buffer;
  unsigned char  name[256], newpath[256];
  U64            found;
  unsigned int   total, pos, nbytes, nchars, newlba, newlen;
  unsigned int   i, j;

  // total sectors
  total = (len + LEN_SECTOR_M0 - 1) / LEN_SECTOR_M0;

  for (i = 0; i < total; i++) {
    // read 1 sector
    buffer = ReadSectors(isoname, lba + i, 1);

    // check the entries in each sector
    pos = 0;
    while (pos < sector_data) {
      // field size
      nbytes = *(unsigned char *)(buffer + data_offset + pos);
      if (!nbytes) break; // no more entries in this sector

      // name size
      nchars = *(unsigned char *)(buffer + data_offset + pos + 0x020);
      for (j = 0; j < nchars; j++) {
        name[j] = *(unsigned char *)(buffer + data_offset + pos + 0x021 + j);
      }
      name[j] = '\0';

      // discard the ";1" final
      if (nchars > 2) {
        if (name[nchars - 2] == ';') {
          nchars -= 2;
          name[nchars] = '\0';
        }
      }

      // check the name except for '.' and '..' entries
      if ((nchars != 1) || ((name[0] != '\0') && (name[0] != '\1'))) {
        // new path name
        sprintf(newpath, "%s/%s", path, name);

        // recursive search in folders
        if (*(unsigned char *)(buffer + data_offset + pos + 0x019) & 0x02) {
          newlba = *(unsigned int *)(buffer + data_offset + pos + 0x002);
          newlen = *(unsigned int *)(buffer + data_offset + pos + 0x00A);

          found = Search(isoname, filename, newpath, newlba, newlen);
          if (found) {
            free(buffer);
            return(found);
          }
        } else {
          // compare names - case insensitive
          for (j = 0; filename[j] && newpath[j]; j++) {
            if ((filename[j] & 0xDF) != (newpath[j] & 0xDF)) break;
          }

          // file found
          if (!filename[j] && !newpath[j]) {
            free(buffer);
            return((U64) (lba + i) * sector_size + data_offset + pos);
          }
        }
      }

      // point to the next entry
      pos += nbytes;
    }

    free(buffer);
  }

  // file not found, return 0
  return(0);
}

/*----------------------------------------------------------------------------*/
void PathTable(char *isoname, int lba, int len, int lba_old, int diff, int sw) {
  unsigned char *buffer;
  unsigned int   total, change, pos, nbytes, newlba;
  unsigned int   i;

  // total sectors
  total = (len + LEN_SECTOR_M0 - 1) / LEN_SECTOR_M0;

  // read all sectors
  buffer = ReadSectors(isoname, lba, total);

  change = 0;
  pos = 0;
  while (pos < len) {
    // field size
    nbytes = *(unsigned char *)(buffer + data_offset + pos);
    if (!nbytes) break; // no more entries in this table

    // position
    newlba = *(unsigned int *)(buffer + data_offset + pos + 0x002);
    if (sw) newlba = ChangeEndian((char *)&newlba);

    // update needed?
    if (newlba > lba_old) {
      change = 1;
      newlba += diff;
      if (sw) newlba = ChangeEndian((char *)&newlba);
      *(unsigned int *)(buffer + data_offset + pos + 0x002) = newlba;
    }

    pos += 0x08 + nbytes + (nbytes & 0x1);
  }

  // update sectors if needed
  if (change) {
    WriteSectors(isoname, lba, buffer, total);
  }

  free(buffer);
}

/*----------------------------------------------------------------------------*/
void TOC(char *isoname, int lba, int len, U64 found, int lba_old, int diff) {
  unsigned char *buffer;
  unsigned char  name[256];
  U64            newfound;
  unsigned int   total, change, pos, nbytes, nchars, newlba, newlen;
  unsigned int   i, j;

  // total sectors
  total = (len + LEN_SECTOR_M0 - 1) / LEN_SECTOR_M0;

  for (i = 0; i < total; i++) {
    // read 1 sector
    buffer = ReadSectors(isoname, lba + i, 1);

    change = 0;
    pos = 0;
    while (pos < len) {
      // field size
      nbytes = *(unsigned char *)(buffer + data_offset + pos);
      if (!nbytes) break; // no more entries in this sector

      // name size
      nchars = *(unsigned char *)(buffer + data_offset + pos + 0x020);
      for (j = 0; j < nchars; j++) {
        name[j] = *(unsigned char *)(buffer + data_offset + pos + 0x021 + j);
      }
      name[j] = '\0';

      // position
      newlba = *(unsigned int *)(buffer + data_offset + pos + 0x002);

      // needed to change a 0-bytes file with more 0-bytes files (same LBA)
      newfound = (U64) (lba + i) * sector_size + data_offset + pos;

      // update needed?
      if ((newlba > lba_old) || ((newlba == lba_old) && (newfound > found))) {
        change = 1;
        newlba += diff;
        j = ChangeEndian((char *)&newlba);

        *(unsigned int *)(buffer + data_offset + pos + 0x002) = newlba;
        *(unsigned int *)(buffer + data_offset + pos + 0x006) = j;
      }

      // check the name except for '.' and '..' entries
      if ((nchars != 1) || ((name[0] != '\0') && (name[0] != '\1'))) {
        // recursive update in folders
        if (*(unsigned char *)(buffer + data_offset + pos + 0x019) & 0x02) {
          newlen = *(unsigned int *)(buffer + data_offset + pos + 0x00A);

          TOC(isoname, newlba, newlen, found, lba_old, diff);
        }
      }

      // point to the next entry
      pos += nbytes;
    }

    // update sector if needed
    if (change) {
      WriteSectors(isoname, lba + i, buffer, 1);
    }

    free(buffer);
  }
}

/*----------------------------------------------------------------------------*/
char *ReadSectors(char *isoname, int lba, int sectors) {
  return(Read(isoname, lba * sector_size, sectors * sector_size));
}

/*----------------------------------------------------------------------------*/
void *WriteSectors(char *isoname, int lba, char *buffer, int sectors) {
  Write(isoname, lba * sector_size, sectors * sector_size, buffer);
}

/*----------------------------------------------------------------------------*/
/*--  EOF                                      Copyright (C) 2012-2015 CUE  --*/
/*----------------------------------------------------------------------------*/

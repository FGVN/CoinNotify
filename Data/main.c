#include <stdio.h>
#include <stdlib.h>
#include <string.h>

int main() {
    FILE *inputFile, *outputFile;

    inputFile = fopen("out.txt", "r");
    if (inputFile == NULL) {
        printf("Failed to open input file.\n");
        return 1;
    }

    outputFile = fopen("fin.txt", "w");
    if (outputFile == NULL) {
        printf("Failed to open output file.\n");
        fclose(inputFile);
        return 1;
    }

    char buf[5000];
    while (!feof(inputFile)) {
      char line[5000]; 
      float price;
      fscanf(inputFile, " %[^\n]", line);

      if (strstr(line, "$<!-- -->") != NULL) {
        sscanf(line, "$<!-- -->%f", &price);

        fprintf(outputFile, "%s\n%f\n", buf, price);
      }

      strcpy(buf, line);
    }

    fclose(inputFile);
    fclose(outputFile);

    printf("Extraction complete. Check 'fin.txt' for the results.\n");

    return 0;
}
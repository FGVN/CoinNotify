#include <stdio.h>
#include <stdlib.h>
#include <string.h>


void extract_span_content(FILE* inputFile, FILE* outputFile) {
    char buffer[1024];
    char* startTag = "<span>";
    char* endTag = "</span>";

    while (fgets(buffer, sizeof(buffer), inputFile) != NULL) {
        char* start = strstr(buffer, startTag);
        char* end;

        while (start != NULL) {
            start += strlen(startTag); // Move the pointer to the end of "<span>"
            end = strstr(start, endTag);

            if (end != NULL) {
                // Copy the content between "<span>" and "</span>" to the output file
                fwrite(start, 1, end - start, outputFile);
                fputc('\n', outputFile); // Add a newline after each instance
            }

            start = strstr(start, startTag); // Move to the next occurrence
        }
    }
}

int main() {
    FILE* inputFile = fopen("coin.txt", "r");
    FILE* outputFile = fopen("out.txt", "w");

    if (inputFile == NULL || outputFile == NULL) {
        perror("Error opening files");
        return 1;
    }

    extract_span_content(inputFile, outputFile);

    fclose(inputFile);
    fclose(outputFile);

    printf("Extraction completed.\n");

  //old

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
        sscanf(line, "$<!-- --> %f", &price);

        fprintf(outputFile, "%s\n%f\n", buf, price);
      }

      strcpy(buf, line);
    }

    fclose(inputFile);
    fclose(outputFile);

    printf("Extraction complete. Check 'fin.txt' for the results.\n");

    return 0;
}
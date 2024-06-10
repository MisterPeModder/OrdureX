TARGET = tests

$(TARGET): binary_tests.o binary.a
	g++ $^ -o $@
binary.a: binary.o
	ar rcs $@ $^
binary.o: ../binary.cpp ../binary.h
	g++ -c -o $@ $<

clean:
	rm -f *.o *.a $(TARGET)

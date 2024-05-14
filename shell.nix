{ pkgs ? import <nixpkgs> {} }:

pkgs.mkShell {
  name = "smart-trash-cans";
  buildInputs = with pkgs; [
    arduino-ide
    arduino-cli
    dotnet-sdk_8
    unityhub
    kicad
  ];
}

# Changelog

All notable changes to this project will be documented in this file.<br>
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

- Added support for mods using Side=NoSync.

## [1.0.2] - 2024-05-04

- Changed project from NET 6.0 to NET 8.0.

## [1.0.1] - 2023-06-10

- Added extension method overloads to read and write easy packets using binary reader and binary writer.

## [1.0.0] - 2023-04-03

- Added `IEasyPacketHandler<T>` generic interface for implementing code to receive a packet.
- Changed project structure: moved internal code into its own directory and namespace.
- Fixed issue with some interfaces not being loaded if generic argument does not match implementing type.
- Fixed loading code not producing an error if the easy packet type isn't declared properly.

## [0.1.1] - 2023-04-01

- Minor update.

## [0.1.0] - 2023-03-26

- Initial release.
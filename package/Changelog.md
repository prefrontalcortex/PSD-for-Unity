# Changelog
All notable changes to this package will be documented in this file.
The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/) and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [0.3.0-exp] - 2021-08-31
- initial release
- imports and exports PSD files with layers, masks, groups
- supports reading lots of extra data from PSD, e.g. some adjustment layer types (writing is limited for now)
- basic implementation of EngineData reading (text layers, fonts, ...)
- basic API for PSD file generation that is pretty manual right now
- some (optional) samples for UIDocument usage as PSD import inspector to analyze files
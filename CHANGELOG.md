# Bond Changelog #

Notable changes--especially new features and breaking changes--are recorded
here.

Bond's version numbers follow [Semantic Versioning](http://semver.org/).
Each release is tagged with a Git tag. The
[NuGet packages](https://www.nuget.org/packages/Bond.CSharp/) follow the Git
tag versions. The Bond compiler (`gbc`) and
[compiler library](https://hackage.haskell.org/package/bond) have a slightly
different versioning scheme, following the Haskell community's
[package versioning policy](https://wiki.haskell.org/Package_versioning_policy).

## Unreleased ##

* `gbc` & compiler library: TBD
* C# NuGet version: TBD
* C# Comm NuGet version: TBD

### Compiler library ###

* User-defined `TypeMapping`s can now be created. This makes is easier to
  implement code generation for new languages. [Pull request
  #172](https://github.com/Microsoft/bond/pull/172)

## 4.2.1: 2016-06-02 ##

* `gbc` & compiler library: 0.4.1.0
* C# NuGet version: 4.2.1
* C# Comm NuGet version: 0.5.0

### `gbc` ###

* Support for parsing
  [service definitions](https://microsoft.github.io/bond/manual/compiler.html#service-definition)
  and generating C# service bases, interfaces, and proxies.
  * New types for
    [services](https://microsoft.github.io/bond/manual/compiler.html#services)
    and
    [methods](https://microsoft.github.io/bond/manual/compiler.html#methods)
    have been added to the compiler AST.
* MSBuild-compatible error messages.
  [Issue #136](https://github.com/Microsoft/bond/issues/136)

### C# ###

* Added
  [`Deserializer.TryDeserialize()`](https://github.com/Microsoft/bond/blob/db315adaf4b812adc5ca484b1bcffafe1df6d351/cs/src/core/Deserializer.cs#L231-239).
* Added two new
  [NuGet packages](https://microsoft.github.io/bond/manual/bond_cs.html#nuget-packages)
  to make it easier to consume Bond piecemeal.
  * Bond.Compiler: contains `gbc`, `bond.bond`, and `bond_const.bond` in a
    tools-only package
  * Bond.Compiler.CSharp: contains `gbc` and C# MSBuild targets. No longer
    do you have to consume Bond.CSharp (which pulls in all of the rest of
    Bond) just to get codegen.
  
### C# Comm ###

* Initial preview release of the
  [Bond Communications framework](https://microsoft.github.io/bond/manual/bond_comm.html).

## 4.2.0: 2016-04-28 ##

* `gbc` & compiler library: 0.4.0.2
* C# NuGet version: 4.2.0

### C# ###

* Add support for Compact Binary v2 writing.
  [Issue #70](https://github.com/Microsoft/bond/issues/70)

## 4.1.0: 2016-04-22

* `gbc` & compiler library: 0.4.0.2
* C# NuGet version: 4.1.0

### `gbc` ###

* Field ordinals larger than 65535 are now rejected.
  [Issue #111](https://github.com/Microsoft/bond/issues/111)
* Fields that duplicate the name of an existing field are now rejected.
  [Issue #123](https://github.com/Microsoft/bond/issues/123)
* The generated C# code now compiles with no errors or warnings at
  `/warn:4`. [Issue #82](https://github.com/Microsoft/bond/issues/82)
* Added
  [Visual Studio Code highlighting rules for `.bond` files](https://github.com/Microsoft/bond/tree/b2b9cd7256286fd484444dfaf7645d380a3ee936/tools/syntax/VsCode).

### C++ ###

* Enums are now cast to 32-bit integers to avoid some compiler warnings.
* Bond can be used in code bases where there is a function-style macro named
  `U`.

### C# ###

* The generated C# code now compiles with no errors or warnings at
  `/warn:4`. [Issue #82](https://github.com/Microsoft/bond/issues/82)
* Bond-generated enums constants are now cast to 32-bit integers to avoid
  some compiler warnings.
* [Implicit conversion between `bond.GUID` and `System.Guid`](https://github.com/Microsoft/bond/blob/bc4c56a3ca0858f4bd93916e80ceff9bbeada606/cs/test/core/GuidConversionTests.cs#L14)
  has been added.
  [Pull request #145](https://github.com/Microsoft/bond/pull/145)
* The ability to
  [customize the implementation of `IBonded<T>` used](https://microsoft.github.io/bond/manual/bond_cs.html#understanding-bondedt)
  has been added.
  [Pull request #153](https://github.com/Microsoft/bond/pull/153)

## 4.0.2: 2015-12-14

* `gbc` & compiler library: 0.4.0.1
* C# NuGet version: 4.0.2

### Bond compiler library ###

* Added a
  [custom codegen example](https://github.com/Microsoft/bond/tree/master/examples/codegen/read-only).

### C# ###

* Added support to
  [not inline nested structs in `Serializers`](https://github.com/Microsoft/bond/blob/cb95fdb3e1e10c3e4cae2f2d55e2b116041010a8/cs/src/core/Serializer.cs#L69).
  This can be used to speed up the time to create the serializer for very
  complex schemas.
* Fix for rare buffer corruption in InputStream.
  [Issue #114](https://github.com/Microsoft/bond/issues/114).
* Fix for SimpleXmlParser not handling XML declarations. [Issue #112](https://github.com/Microsoft/bond/issues/82)

## Earlier release ##

These sections need to be written. In the meantime, consult the
[repository's history](https://github.com/Microsoft/bond/commits/master).

# OmenMon

<p align="center"><a href="https://omenmon.github.io/">Project Page &amp; Documentation</a> • <a href="https://omenmon.github.io/build">Building Instructions</a> • <a href="https://github.com/OmenMon/Localization">Translation Repository</a> • <a href="https://github.com/OmenMon/OmenMon/releases/latest">Download Latest Release ⭳</a></p>

![OmenMon graphical mode overview](https://omenmon.github.io/pic/gui-overview.png)

**OmenMon** is a lightweight application that interacts with the Embedded Controller (EC) and WMI BIOS routines of an _HP Omen_ laptop in order to access hardware settings, in particular to [query temperature sensors](https://omenmon.github.io/gui#temperature) and dynamically [adjust fan speeds](https://omenmon.github.io/gui#fan-control). It also helps you pick your favorite [keyboard backlight colors](https://omenmon.github.io/gui#keyboard) and put the [_Omen_ key](https://omenmon.github.io/config#key) to better use.

**OmenMon** endeavors to replace all the useful functionality of the _Omen Hub_ (a.k.a. _Omen Control Center_), the laptop manufacturer's application, without any of its numerous anti-features. It does not connect to the network at all, does not have advertising, built-in store, social-media integration and whatnot. It does only what you expect it to do and nothing else.

**OmenMon** is designed to run with minimal resource overhead. It comes with a clear and compact [graphical interface](https://omenmon.github.io/gui), offering a great degree of [configurability](https://omenmon.github.io/config) while also featuring an extensive [command-line mode](https://omenmon.github.io/cli) where various BIOS and EC read and write operations can be performed manually. 

Most features are specific to _HP_ devices with a compatible BIOS interface exposed by the `ACPI\PNP0C14` driver but command-line [Embedded Controller operations](https://omenmon.github.io/cli#ec) should work on all laptops.

Full documentation is available at [https://omenmon.github.io/](https://omenmon.github.io/)

_This software is not affiliated with or endorsed by HP. Any brand names are used for informational purposes only._

## Version History

### 0.54 (2023-11-06)

  * Make platform fan and temperature array setup model-dependent
  * Make BIOS calls to retrieve GPU mode not raise an exception on unsupported models, hide the menu items related to GPU mode in such scenarios
  * Add `GuiDpiChangeResize` configuration option to set whether the window should be automatically resized in response to DPI changes
  * Add `GuiSysInfoFontSize` configuration option to override the font size used for _System Information & Status_

### 0.53 (2023-11-06)

  * Update missing localization string for _Unknown_ throttling status (introduced in 0.51)

### 0.52 (2023-11-05)

  * Fix `DynamicIcon` and `DynamicIconHasBackground` configuration settings not being saved
  * Resolve the issue when unless the main window is being shown, temperature sensors are not updated before calculating maximum temperature. Thank you to **[@wangzhengbin](https://github.com/wangzhengbin)** for reporting this issue.

### 0.51 (2023-11-05)

  * Resolve the issue when a BIOS call to check throttling status results in an unhandled exception where not supported. The call is not supported on 2023 models where it yields BIOS error code 6. The status will now be reported as _Unknown_ in these scenarios. Thank you to **[@breadeding](https://github.com/breadeding)** for contributing information that made it possible to fix this issue.
  * Main window title consistency fix

### 0.50 (2023-11-04)

  * Initial public preview
  * Publish a complete documentation at [omenmon.github.io](https://omenmon.github.io/)
  * Publish an XML translation template at [github.com/OmenMon/Localization](https://github.com/OmenMon/Localization)
  * Detect _PowerShell_ console to workaround output issues

**OmenMon** is feature-complete and fully works on the _HP Omen_ `8A14` (`Ralph 22C1` or _Omen 16_) platform I can test it with on Windows 10 `21H2` (`10.0.19044`). Since all the functionality is very hardware-specific, there might be issues on other platforms different from the one I have, which will have to be ironed out. Thus, only a preview release for now but it _should_ fully work – as long as your laptop is similar enough to mine.

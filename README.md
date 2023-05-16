# koeiromap-api-unity

A client library for [Koeiromap API](http://koeiromap.rinna.jp/) for Unity.

## How to import by Unity Package Manager

Add following dependencies to your `manifest.json`:

```json
{
  "dependencies":
  {
    "com.mochineko.koeiromap-api": "https://github.com/mochi-neko/koeiromap-api-unity.git?path=/Assets/Mochineko/KoeiromapAPI#0.1.0",
    "com.mochineko.relent": "https://github.com/mochi-neko/Relent.git?path=/Assets/Mochineko/Relent#0.2.0",
    "com.mochineko.relent.extensions.newtonsoft-json": "https://github.com/mochi-neko/Relent.git?path=/Assets/Mochineko/Relent.Extensions/NewtonsofJson#0.2.0",
    "com.cysharp.unitask": "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask",
    ...
  }
}
```
.

## How to use

See [sample implementation](./Assets/Mochineko/KoeiromapAPI.Samples/KoeiromapAPISample.cs).

## Change log

See [CHANGELOG.md](./CHANGELOG.md)

## 3rd party notices

See [NOTICE.md](./NOTICE.md)

## License

Licensed under the [MIT License](./LICENSE).
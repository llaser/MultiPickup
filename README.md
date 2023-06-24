# MultiPickup

複数のオブジェクトをまとめて動かすためのU#スクリプト

## Requirements

- VRChat SDK 3.x (3.2.0で動作確認)
- UdonSharp 1.x (3.1.8で動作確認)

## 各スクリプトについて

### MPPicker

これをアタッチした`GameObject`で`MPPickup`をアタッチした`GameObject`を動かします。何らかの方法で`AttachOverlappingMPPickups()`を実行することで、設定したコライダーに接触している`MPPickup`をchildにします。

- parentを弄る都合上、これをアタッチする`GameObject`のScaleの各要素は同じ値でないと歪みます

`MPPickup`をアタッチした`GameObject`は同期を行わない、いわゆるローカルなオブジェクトとすることを推奨します。
`VRCObjectSync`等で同期する場合には、`MPPickup`と衝突をしないように設定することを推奨します。

### MPPickerOnUse

`OnPickupUseDown()`で`AttachOverlappingMPPickups()`を実行する`MPPicker`

### MPPickup

MPPickerによって移動されるGameObjectにアタッチすると、`MPPicker`によって移動出来るようになります。

同期を行う場合はVRCObjectSyncで行うことを想定しています。
ownershipの取得・喪失時に`Rigidbody.isKinematic`を制御しているため、必要に応じて`DontControlKinematic`を有効にしてください。

## Auther

llaser

## License

This work is licensed under the MIT license. See LICENSE for details.
Copyright (c) 2023 llaser

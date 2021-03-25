# OldMusicBox.EIH.Client

The goal of this project is to provide an independent .NET EIH (Electronic Identification Hub, **Węzeł Krajowy**) Client. 
The client will support the Węzeł Krajowy SSO flow

The implementation follows the 
[official specification](https://mc.bip.gov.pl/interoperacyjnosc-mc/wezel-krajowy-dokumentacja-dotyczaca-integracji-z-wezlem-krajowym.html).

## Current Version: 0.5

Please refer to the change list and the road map below.

## Features:

|  Feature  | Status |
|----|:---:|
|Client demo|**yes**|
|Server demo|**yes**|
|NuGet|**yes**|
|Single Sign On|**yes**|
|Single Log Out|**yes**|
|.NET Framework|4.6.2+|
|.NET Core|not yet|

## Documentation

### EIH (Węzeł Krajowy) SSO

Węzeł Krajowy SSO is based on SAML2 ARTIFACT binding. The implementation follows the eIDAS (Electronic Identification and Trust Services Regulation) regulation when it comes to cryptography:

* requests and responses are signed using the ECDSA private keys 
(Elliptic Curve Digital Signature Algorithm)
* the elliptic curve used in private keys is the 
NIST Curve P-256
* the assertion returned from the ArtifactResolve call is signed using:
   * AES256-GCM – data signing
   * AES-256-KW – key signing
   * the key is protected with the ECDH-ES Key Agreement protocol

### EIH (Węzeł Krajowy) in .NET

Węzeł Krajowy in .NET is difficult because

* the base class library doesn't contain the SAML2 client
* the base class library doesn't support the ECDSA in **SignedXml**

Because of this, [BouncyCastle](https://github.com/bcgit/bc-csharp) must be used and because of the **SignedXml** integration, a significant part of the **SignedXml** has to be rewritten to switch the crypto subsystem. Fortunately, someone actually did it and there's the [bc-xml-security](https://github.com/kmvi/bc-xml-security) package (which still misses the support of ECDSA but it's easily fixable).

The SAML2 is based on the [OldMusicBox.SAML2](https://github.com/wzychla/OldMusicBox.Saml2) and my ultimate goal is to have a single SAML2 stack where the cryptography is abstracted so that you can switch between the Base Class Library that does RSA and BouncyCastle if you also need ECDSA.

Still, the most difficult part of the implementation is the encrypted assertion decryption. The Centralny Ośrodek Informatyki publishes a source code of a Java decryptor. The core part of their decryptor uses few different Java packages. The tricky part here
was then to rewrite the Java code to .NET - it was fairly
straightforward to rewrite the BouncyCastle code, however
it was not that easy to rewrite the **org.apache.xml.security.encryption.XMLCipher** code. 

## Repository content

### OldMusicBox.EIH.Client

This is the client/server library that lets you develop your own Węzeł Krajowy clients as well as stub Węzeł Krajowy servers.

### OldMusicBox.EIH.Demo

This is the demo **client application**. It performs SAML2 Artifact binding flow against the Węzeł Krajowy. It demonstrates key steps that have to be implemented in a client app:

* it creates the `AuthnRequest` and sends it to the server
* it parses the `AuthnResponse` and extracts the **artifact**
* it creates the `ArtifactResolve` and sends it to the server
* it parses the `ArtifactResponse` and decrypts the encrypted assertion

### OldMusicBox.EIH.ServerDemo

This is the demo **server application**. It follows the server implementation close enough to let you test your own client implementations. In particular:

* it reads client's `AuthnRequest` and creates `AuthnResponse`
* it reads client's `ArtifactResolve` and creates `ArtifactResponse` with encrypted assertion

| **Important!** |
|----------------|
|Server demo application is very useful. In particular, you can use it to quickly validate any client implementation, including implementations developed in other technology stacks (node.js, Java, PHP). If you plan to integrate with Węzeł Krajowy, consider this server demo as a preliminary Węzeł Krajowy test enviroment. |

## Version History:

* 0.6 (2021-03-25)
    * server demo works

* 0.51 (2021-03-08)
    * server demo slowly gets into shape

* 0.5 (2020-12-18)
    * assertion decryption test passes
    * SSO, SLO works correctly
    * demo included

* 0.1 (2020-12-18)
    * initial commit


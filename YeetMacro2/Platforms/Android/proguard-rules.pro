# ProGuard/R8 rules for YeetMacro2 Android Release builds

# Keep .NET MAUI and Xamarin classes
-keep class mono.** { *; }
-keep class Microsoft.Maui.** { *; }
-keep class androidx.** { *; }
-keep class com.google.android.material.** { *; }

# Keep application classes
-keep class com.yeetoverflow.yeetmacro2.** { *; }

# Keep CommunityToolkit.Mvvm generated classes and properties
-keep class * implements CommunityToolkit.Mvvm.ComponentModel.ObservableObject { *; }
-keepclassmembers class * {
    @CommunityToolkit.Mvvm.Input.RelayCommand *;
    @CommunityToolkit.Mvvm.ComponentModel.ObservableProperty *;
}

# Keep ViewModels
-keep class YeetMacro2.ViewModels.** { *; }
-keep class YeetMacro2.Platforms.Android.ViewModels.** { *; }

# Keep Data Models
-keep class YeetMacro2.Data.Models.** { *; }

# Keep Services
-keep class YeetMacro2.Services.** { *; }
-keep class YeetMacro2.Platforms.Android.Services.** { *; }

# Keep Views
-keep class YeetMacro2.Views.** { *; }
-keep class YeetMacro2.Pages.** { *; }
-keep class YeetMacro2.Platforms.Android.Views.** { *; }

# Keep Converters
-keep class YeetMacro2.Converters.** { *; }

# Keep attributes for reflection
-keepattributes *Annotation*
-keepattributes Signature
-keepattributes InnerClasses
-keepattributes EnclosingMethod
-keepattributes Exceptions

# Keep Entity Framework Core models
-keep class Microsoft.EntityFrameworkCore.** { *; }
-keep class SQLite.** { *; }

# Keep Jint JavaScript engine
-keep class Jint.** { *; }

# Keep SkiaSharp
-keep class SkiaSharp.** { *; }

# Keep OpenCV
-keep class org.opencv.** { *; }

# Keep Tesseract OCR
-keep class com.googlecode.tesseract.** { *; }
-keep class com.googlecode.leptonica.** { *; }

# Keep serialization classes
-keepclassmembers class * implements java.io.Serializable {
    static final long serialVersionUID;
    private static final java.io.ObjectStreamField[] serialPersistentFields;
    private void writeObject(java.io.ObjectOutputStream);
    private void readObject(java.io.ObjectInputStream);
    java.lang.Object writeReplace();
    java.lang.Object readResolve();
}

# Keep native methods
-keepclasseswithmembernames class * {
    native <methods>;
}

# Keep enums
-keepclassmembers enum * {
    public static **[] values();
    public static ** valueOf(java.lang.String);
}

# Keep Parcelable implementations
-keepclassmembers class * implements android.os.Parcelable {
    public static final android.os.Parcelable$Creator CREATOR;
}

# Suppress warnings for missing classes in dependencies
-dontwarn org.slf4j.**
-dontwarn javax.**
-dontwarn java.awt.**
-dontwarn sun.misc.**

# Optimization settings
-optimizationpasses 5
-dontpreverify
-repackageclasses ''
-allowaccessmodification
-optimizations !code/simplification/arithmetic,!code/simplification/cast,!field/*,!class/merging/*
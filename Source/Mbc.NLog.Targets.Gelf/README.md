# Gelf Target

Stellt ein NLog-Target zur Verfügung um Log-Meldungen über GELF an ein 
Netzwerk-Ziel wie z.B. Graylog zu versenden.

## Configuration

```
<targets>
    <target xsi:type="Gelf"
            name="Gelf"
            layout="Layout"
            layout_full="Layout"
            includeEventProperties="true|false"
            includeGdc="true|false"
            includeMdc="true|false"
            includeMdlc="true|false"
            includeNdc="true|false"
            includeNdlc="true|false"
            endpoint="udp://<host>:<port>|tcp://<host>:<port>">
        <contextproperty name="Name"
                         layout="Layout" />
    </target>
</targets>
```


## Parameters

### Generel

*  **name** - Name des Targets

### GELF Message Optionen

*  **layout** - Layout des Standardtextes (GELF: *short_message*). Default: `${message}`
*  **layout_full** - Layout des erweiterten Textes (GELF: *full_message*). Default: `""`
*  **includeEventProperties** - Aktiviert das *Structured Logging*, siehe auch https://messagetemplates.org/. Die Properties werden der GELF-Message direkt hinzugefügt. Default: `true`
*  **includeGdc** - Aktiviert den *Global Diagnostic Context*. Die Properties werden der GELF-Message direkt hinzugefügt. Default: `true`
*  **includeMdc** - Aktiviert den *Mapped Diagnostic Context*. Die Properties werden der GELF-Message direkt hinzugefügt. Default: `true`
*  **includeMdlc** - Aktiviert den *Mapped Diagnostic Logical Context*. Die Properties werden der GELF-Message direkt hinzugefügt. Default: `true`
*  **includeNdc** - Aktiviert den *Nested Diagnostic Context*. Die Properties werden der GELF-Message direkt hinzugefügt. Default: `true`
*  **includeNdlc** - Aktiviert den *Nested Diagnostic Logical Context*. Die Properties werden der GELF-Message direkt hinzugefügt. Default: `true`

### Übertragungs Optionen

* **endpoint** - Der Entpoint der GELF-Message. Es werden tcp/udp-Protokolle unterstützt. Default: `udp://localhost:12201`

## GELF-Message

### Log-Properties

Folgende Felder werden automatisch generiert:

* **log_exception_message** - Exception.Message wenn vorhanden
* **log_exception_stacktrace** - Exception.StackTrace wenn vorhanden
* **log_loggername** - Der Logger-Name

### Log-Level

Der Log-Level wird wie folgt übertragen:

* Fatal = 2 (Critical)
* Error = 3 (Error)
* Warn = 4 (Warn)
* Info = 6 (Informational)
* Debug = 7 (Debug)

Der Trace-Level wird **nicht** übertragen.

## Additional-Fields

Per Default werden alle Properties als GELF-Additional-Field übertragen. Um Kollisionen zu vermeiden werden die Properties von NLog wie folgt umbenannt:

* Alle Event-Properties (Argumente der Message) werden direkt mit ihrem Namen übertragen.
* Context-, GDC-, MDC-, MDLC-Properties werden mit dem Präfix `log_prop_` versehen.
* NDLC bzw. NDC werden als `log_ndlc` bzw. `log_ndc` übertragen


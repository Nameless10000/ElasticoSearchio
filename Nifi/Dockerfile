FROM apache/nifi:2.7.2

# NARs
COPY nars/*.nar /opt/nifi/nifi-current/nar_extensions/

# MariaDB JDBC driver
COPY drivers/mariadb-java-client-3.3.3.jar \
     /opt/nifnifi-current/drivers/
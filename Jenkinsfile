node {
    stage('Clone repository') {
        git branch: 'main', credentialsId: 'github-app-SilKsPlugins', url: 'https://github.com/SilKsPlugins/SilKsPlugins.DiscordBot'
    }
    
    stage('Build image') {
        app = docker.build("silksplugins-discordbot")
    }
    
    stage('Push image') {
        docker.withRegistry('http://127.0.0.1:6000') {
            app.push("0.1.${env.BUILD_NUMBER}")
            app.push('latest')
        }
    }
    
    stage('Deploy container') {
        sh '''
            # Stop silksplugins-discordbot docker if it's running
            docker ps -q --filter "name=silksplugins-discordbot" | grep -q . \
                && docker stop silksplugins-discordbot

            # Remove silksplugins-discordbot docker if it exists
            docker ps -a -q --filter "name=silksplugins-discordbot" | grep -q . \
                && docker rm -fv silksplugins-discordbot

            # Create silksplugins-discordbot network if it doesn't exist
            docker network ls -q --filter "name=silksplugins-discordbot" | grep -q . \
                || docker network create silksplugins-discordbot

            # Create and start nbcovidbot container
            docker run -d \
                -v silksplugins-discordbot:/data \
                --network silksplugins-discordbot \
                --name silksplugins-discordbot \
                silksplugins-discordbot:latest
        '''
    }

    stage('Connect MariaDb container to network') {
        sh '''
            # Connect mariadb-main to network if not already connected
            docker network inspect silksplugins-discordbot \
                -f "{{ range .Containers }}{{.Name}} {{ end }}" | grep -q mariadb-main \
                || docker network connect silksplugins-discordbot mariadb-main
        '''
    }
}
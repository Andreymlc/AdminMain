pipeline {
    agent any
    
    parameters {
        booleanParam(name: 'DEPLOY', defaultValue: false, description: 'Run deploy stage (start/refresh containers)')
    }
    
    environment {
        DOCKER_COMPOSE = 'docker compose'
        PROJECT_DIR = "${WORKSPACE}"
        COMPOSE_PROJECT_NAME = 'admin-microservices'
    }
    
    stages {
        stage('Checkout') {
            steps {
                echo 'Checking out code from repository...'
                checkout scm
            }
        }
        
        stage('Build') {
            steps {
                echo 'Building Docker images...'
                script {
                    dir("${PROJECT_DIR}") {
                        sh '''
                            docker compose -p admin-microservices build --no-cache gateway-service mediator-service data-service
                        '''
                    }
                }
            }
        }
        
        stage('Deploy') {
            when {
                expression { return params.DEPLOY == true }
            }
            steps {
                echo 'Deploying services...'
                script {
                    dir("${PROJECT_DIR}") {
                        sh '''
                            docker compose -p admin-microservices up -d gateway-service mediator-service data-service prometheus grafana
                        '''
                    }
                }
            }
        }
    }
    
    post {
        success {
            echo 'Pipeline succeeded!'
            script {
                echo "Build completed successfully at ${new Date()}"
            }
        }
        failure {
            echo 'Pipeline failed!'
            script {
                echo "Build failed at ${new Date()}"
            }
        }
        always {
            echo 'Pipeline finished.'
            cleanWs()
        }
    }
}


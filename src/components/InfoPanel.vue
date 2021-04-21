<template>
  <div>
    <!--<b-button v-b-toggle.sidebar-1>Toggle Sidebar</b-button>-->
    <b-sidebar
      id="sidebar-1"
      title="Sidebar"
      right
      shadow
      no-header-close="true"
      visible="visible">
      <div class="px-3 py-2">
        <p>
          Cras mattis consectetur purus sit amet fermentum. Cras justo odio, dapibus ac facilisis
          in, egestas eget quam. Morbi leo risus, porta ac consectetur ac, vestibulum at eros.
        </p>
        <b-img src="https://picsum.photos/500/500/?image=54" fluid thumbnail></b-img>
      </div>
    </b-sidebar>
  </div>
</template>

<script>
import SidebarPanel from 'bootstrap-vue'



//import 'node_modules/bootstrap/scss/bootstrap.scss';
//import 'node_modules/bootstrap-vue/src/index.scss';


export default {
  name: "InfoPanel",
  components: { SidebarPanel }

}
</script>

<style scoped>

</style>
